import { Observable, filter, map } from "rxjs"
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
    private receivedOccurenceByMatter: Map<string, ArchivedReceivedMatterOccurence> = new Map()

    newMatterOccurenceProcessed: Observable<ArchivedMatterOccurence>

    constructor(newOccurenceObservable: Observable<IRawOccurence>) {
        this.newMatterOccurenceProcessed = newOccurenceObservable.pipe(
            filter(occ => occ.occurenceCategory === OccurenceCategory.Matter),
            map(occ => this.process(occ as IRawMatterOccurence)),
            filter((occ): occ is ArchivedMatterOccurence => occ !== null),
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
        const matterGuid = occ.receivedMatterGuid

        // Skip received occurrences that reference unknown matter — shaped must
        // arrive first (which it always does in practice).
        if (!this.matterMap.has(matterGuid)) return null

        this.matterMap.get(matterGuid)?.addReceivingSpell(occ.spellGuid)

        // All receivals of the same matter instance collapse into one list entry,
        // regardless of what other occurrences land between them.  A downstream
        // reaction emitting its own shaped matter used to break the old
        // "consecutive last" check and produce duplicate Received rows.
        const existing = this.receivedOccurenceByMatter.get(matterGuid)
        if (existing) {
            existing.addReceivingSpell(occ.spellGuid)
            return null
        }

        const newOccurence = new ArchivedReceivedMatterOccurence(
            occ.guid,
            occ.timestamp,
            occ.matterOccurenceCategory,
            matterGuid,
            occ.spellGuid,
        )
        this.receivedOccurenceByMatter.set(matterGuid, newOccurence)
        return newOccurence
    }

    getMatterShapingSpellGuid(matterGuid: string): string | undefined {
        return this.matterMap.get(matterGuid)?.spellReference
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
