import { Observable, combineLatest, filter, map, merge, of, tap } from "rxjs"
import { ArchivedMatter } from "../common/matter/ArchivedMatter"
import { ArchivedMatterOccurence, ArchivedReceivedMatterOccurence, ArchivedShapedMatterOccurence } from "../occurences-archived/ArchivedMatterOccurence"
import { MatterData } from "../common/matter/MatterData"
import { Type } from "../common/Type"
import { MatterOccurenceCategory } from "../occurence-categories/MatterOccurenceCategory"
import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"
import { IRawReceivedMatterOccurence } from "../occurences-raw/matter/IRawReceivedMatterOccurence"
import { IRawShapedMatterOccurence } from "../occurences-raw/matter/IRawShapedMatterOccurence"
import { IRawMatterOccurence } from "../occurences-raw/matter/IRawMatterOccurence"
import { IRawOccurence } from "../occurences-raw/IRawOccurence"
import { IArchivedMatterOccurence } from "../occurences-archived/IArchivedMatterOccurence"

export class MatterArchive {
    private matterMap: Map<string, ArchivedMatter> = new Map<
        string,
        ArchivedMatter
    >()

    // TODO to keep last stateful matter
    private lastMatterOfState: Map<Type, string> = new Map<Type, string>()

    private lastArchivedMatterOccurence: ArchivedMatterOccurence

    newMatterOccurenceProcessed: Observable<ArchivedMatterOccurence>

    constructor(newOccurenceObservable: Observable<IRawOccurence>) {
        const rawMatterOccurenceObservable = newOccurenceObservable.pipe(
            filter(occ => occ.occurenceCategory == OccurenceCategory.Matter),
            map(occ => {
                return occ as IRawMatterOccurence
            })
        )

        const processedShapedMatterObservable =
            this.processShapedMatterOccurences(rawMatterOccurenceObservable)

        const processedReceivedMatterObservable =
            this.processReceivedMatterOccurences(rawMatterOccurenceObservable)

        this.newMatterOccurenceProcessed = merge(
            processedShapedMatterObservable,
            processedReceivedMatterObservable
        ).pipe(tap(occ => (this.lastArchivedMatterOccurence = occ)))
    }

    private processReceivedMatterOccurences(
        newMatterOccurenceObservable: Observable<IRawMatterOccurence>
    ): Observable<ArchivedMatterOccurence> {
        return newMatterOccurenceObservable.pipe(
            filter(_ => this.lastArchivedMatterOccurence !== undefined),
            filter(
                occ =>
                    occ.matterOccurenceCategory ==
                    MatterOccurenceCategory.Received
            ),
            map(occ => occ as IRawReceivedMatterOccurence),
            filter(occ => occ !== undefined),
            map(occ => {
                const matterGuid = occ.receivedMatterGuid

                const canBeCollapsed =
                    this.lastArchivedMatterOccurence.matterOccurenceCategory ===
                        MatterOccurenceCategory.Received &&
                    this.lastArchivedMatterOccurence.matterGuid === matterGuid

                return { occ, canBeCollapsed, matterGuid }
            }),
            tap(x => {
                this.addReceivedBy(x.matterGuid, x.occ.spellGuid)

                if (x.canBeCollapsed === true)
                {
                    const receivedOccurence = 
                        this.lastArchivedMatterOccurence as ArchivedReceivedMatterOccurence

                    receivedOccurence.addReceivingSpell(x.occ.spellGuid)
                }
            }),
            filter(x => x.canBeCollapsed === false),
            map(x => {
                const processedMatterOccurence = new ArchivedReceivedMatterOccurence(
                    x.occ.guid,
                    x.occ.timestamp,
                    x.occ.matterOccurenceCategory,
                    x.matterGuid,
                    x.occ.spellGuid,
                )

                return processedMatterOccurence
            })
        )
    }

    private addMatter(
        matterGuid: string,
        matterType: Type,
        matter: MatterData,
        shapingSpellReference: string
    ) {
        const knownMatter: ArchivedMatter = new ArchivedMatter(
            matterType,
            matter,
            shapingSpellReference
        )

        this.matterMap.set(matterGuid, knownMatter)
    }

    private processShapedMatterOccurences(
        newMatterOccurenceObservable: Observable<IRawMatterOccurence>
    ): Observable<ArchivedMatterOccurence> {
        return newMatterOccurenceObservable.pipe(
            filter(
                occ =>
                    occ.matterOccurenceCategory ==
                    MatterOccurenceCategory.Shaped
            ),
            map(occ => occ as IRawShapedMatterOccurence),
            filter(occ => occ !== undefined),
            tap(occ => {
                /* 🌄🗃️ SIDE EFFECTS */
                // Before new Matter Occurence can be rendered it needs to be saved here

                const matterGuid = occ.matter.guid

                this.addMatter(
                    matterGuid,
                    occ.matterType,
                    occ.matter,
                    occ.spellGuid
                )
            }),
            map(occ => {
                const matterGuid = occ.matter.guid

                const newArchivedMatterOccurence = new ArchivedShapedMatterOccurence(
                    occ.guid,
                    occ.timestamp,
                    occ.matterOccurenceCategory,
                    matterGuid,
                    occ.spellGuid,
                )

                return newArchivedMatterOccurence
            })
        )
    }

    private getKnownMatter(matterGuid: string): ArchivedMatter | undefined {
        const knownMatter = this.matterMap.get(matterGuid)

        if (knownMatter === undefined) {
        }

        return knownMatter
    }

    deleteMatter(matterGuid: string) {
        this.matterMap.delete(matterGuid)
    }

    addReceivedBy(matterGuid: string, occurenceGuid: string) {
        this.matterMap.get(matterGuid)?.addReceivedBy(occurenceGuid)
    }

    getMatterType(matterGuid: string): Type {
        const matter = this.matterMap.get(matterGuid)
        if (matter === undefined) {
            return new Type(
                "UNDEFINED",
                `Unknown namespace for matter guid: ${matterGuid}`
            )
        } else {
            return matter.matterType
        }
    }

    getMatterData(matterGuid: string): MatterData {
        const matter = this.matterMap.get(matterGuid)
        if (matter === undefined) {
            return {
                guid: matterGuid,
                description: "Unknown",
                circumstances: [],
                content: {},
            }
        } else {
            return matter.matterData
        }
    }

    getMatterReceivedCountObservable(matterGuid: string): Observable<number> {
        const matter = this.matterMap.get(matterGuid)
        if (matter === undefined) {
            return of(-1)
        } else {
            return matter.receivedCountObservable
        }
    }
}
