import { Observable, filter, map, tap } from "rxjs"
import { ArchivedMatter } from "../common/matter/ArchivedMatter"
import {
    ArchivedMatterOccurence,
    ArchivedReceivedMatterOccurence,
    ArchivedShapedMatterOccurence,
} from "../occurences-archived/ArchivedMatterOccurence"
import { MatterData } from "../common/matter/MatterData"
import { Type } from "../common/Type"
import { MatterOccurenceCategory } from "../occurence-categories/MatterOccurenceCategory"
import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"
import { IRawReceivedMatterOccurence } from "../occurences-raw/matter/IRawReceivedMatterOccurence"
import { IRawShapedMatterOccurence } from "../occurences-raw/matter/IRawShapedMatterOccurence"
import { IRawMatterOccurence } from "../occurences-raw/matter/IRawMatterOccurence"
import { IRawOccurence } from "../occurences-raw/IRawOccurence"

export class MatterArchive {
    private matterMap: Map<string, ArchivedMatter> = new Map()
    private lastArchivedMatterOccurence: ArchivedMatterOccurence | undefined

    newMatterOccurenceProcessed: Observable<ArchivedMatterOccurence>

    constructor(newOccurenceObservable: Observable<IRawOccurence>) {
        this.newMatterOccurenceProcessed = newOccurenceObservable.pipe(
            filter(occ => occ.occurenceCategory === OccurenceCategory.Matter),
            map(occ => this.process(occ as IRawMatterOccurence)),
            filter((occ): occ is ArchivedMatterOccurence => occ !== null),
            tap(occ => (this.lastArchivedMatterOccurence = occ)),
        )
    }

    private process(occ: IRawMatterOccurence): ArchivedMatterOccurence | null {
        switch (occ.matterOccurenceCategory) {
            case MatterOccurenceCategory.Shaped:
                return this.processShaped(occ as IRawShapedMatterOccurence)
            case MatterOccurenceCategory.Received:
                return this.processReceived(occ as IRawReceivedMatterOccurence)
            default:
                return null
        }
    }

    private processShaped(
        occ: IRawShapedMatterOccurence,
    ): ArchivedShapedMatterOccurence {
        this.matterMap.set(
            occ.matter.guid,
            new ArchivedMatter(occ.matterType, occ.matter, occ.spellGuid),
        )
        return new ArchivedShapedMatterOccurence(
            occ.guid,
            occ.timestamp,
            occ.matterOccurenceCategory,
            occ.matter.guid,
            occ.spellGuid,
        )
    }

    private processReceived(
        occ: IRawReceivedMatterOccurence,
    ): ArchivedReceivedMatterOccurence | null {
        // Skip received occurrences that arrive before any shaped occurrence —
        // there is no last occurrence to collapse against and nothing has been
        // archived yet for them to reference.
        if (this.lastArchivedMatterOccurence === undefined) return null

        const matterGuid = occ.receivedMatterGuid
        this.matterMap.get(matterGuid)?.addReceivingSpell(occ.spellGuid)

        // 🐖 the collapsing functions in the main matter list window as a little anti-spam step
        // instead of listing every single receival of a given shaped matter, a little counter
        // will be displayed on the received matter slot + when unfolded will also show a list
        // of all spells that received it before another matter occurence happened in rzeka
        const last = this.lastArchivedMatterOccurence
        if (
            last.matterOccurenceCategory === MatterOccurenceCategory.Received &&
            last.matterGuid === matterGuid
        ) {
            const lastReceived = last as ArchivedReceivedMatterOccurence
            lastReceived.addReceivingSpell(occ.spellGuid)
            return null
        }

        return new ArchivedReceivedMatterOccurence(
            occ.guid,
            occ.timestamp,
            occ.matterOccurenceCategory,
            matterGuid,
            occ.spellGuid,
        )
    }

    hasMatter(matterGuid: string): boolean {
        return this.matterMap.has(matterGuid)
    }

    getMatterType(matterGuid: string): Type {
        const matter = this.matterMap.get(matterGuid)
        if (matter === undefined) {
            console.error(
                `MatterArchive: type lookup for unknown matter ${matterGuid}`,
            )
            return new Type(
                "⚠ MISSING",
                `No archived matter for guid ${matterGuid}`,
            )
        }
        return matter.matterType
    }

    getMatterData(matterGuid: string): MatterData {
        const matter = this.matterMap.get(matterGuid)
        if (matter === undefined) {
            console.error(
                `MatterArchive: data lookup for unknown matter ${matterGuid}`,
            )
            return {
                guid: matterGuid,
                description: "⚠ MISSING",
                circumstances: [],
                content: {},
            }
        }
        return matter.matterData
    }
}
