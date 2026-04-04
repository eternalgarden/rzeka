import { Observable, merge, fromEvent, map, filter } from "rxjs"
import { deserializeMatterOccurence } from "../../serialization/deserialize-matter-occurences"
import { deserializeMessageOccurence } from "../../serialization/deserialize-message-occurence"
import { deserializeSpellOccurence } from "../../serialization/deserialize-spell-occurence"
import { ON_NEW_RAW_OCCURENCE } from "../common/ErisEvents"
import { MessageArchive } from "./MessageArchive"
import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"
import { IArchivedOccurence } from "../occurences-archived/IArchivedOccurence"
import { IRawOccurence } from "../occurences-raw/IRawOccurence"
import { MatterArchive } from "./MatterArchive"
import { SpellArchive } from "./SpellArchive"

export class OccurencesArchive {
    private readonly occurenceMemoryLimit: number
    readonly spellsArchive: SpellArchive
    readonly matterArchive: MatterArchive
    readonly messageArchive: MessageArchive

    private allOccurencesArchive: Map<string, IArchivedOccurence> = new Map<
        string, // GUID
        IArchivedOccurence
    >()
    private occurenceReferenceQueue: string[] = []

    newProcessedOccurence: Observable<IArchivedOccurence>

    constructor(occurenceMemoryLimit: number) {
        this.occurenceMemoryLimit = occurenceMemoryLimit

        const newOccurencesObservable = this.onNewOccurenceObservable()

        this.matterArchive = new MatterArchive(newOccurencesObservable)
        this.spellsArchive = new SpellArchive(newOccurencesObservable)
        this.messageArchive = new MessageArchive(newOccurencesObservable)

        this.newProcessedOccurence = merge(
            this.matterArchive.newMatterOccurenceProcessed,
            this.spellsArchive.newSpellOccurenceProcessed,
            this.messageArchive.newMessageOccurenceProcessed
        ).pipe()
    }

    // 🐺 Private
    onNewOccurenceObservable(): Observable<IRawOccurence> {
        return fromEvent(document, ON_NEW_RAW_OCCURENCE).pipe(
            map(x => {
                const next = x as CustomEvent
                return next.detail
            }),
            map(json => {
                // console.log("oik")
                let newOccurence: IRawOccurence | undefined

                if (json.occurenceCategory == OccurenceCategory.Spell) {
                    newOccurence = deserializeSpellOccurence(json)
                } else if (json.occurenceCategory == OccurenceCategory.Matter) {
                    newOccurence = deserializeMatterOccurence(json)
                } else if (
                    json.occurenceCategory == OccurenceCategory.Message
                ) {
                    newOccurence = deserializeMessageOccurence(json)
                } else {
                    throw new Error(
                        `Unknown Occurence Category: ${json.occurenceCategory}`
                    )
                }

                return newOccurence
            }),
            filter(occurence => occurence !== undefined),
            map(occ => occ as IRawOccurence)
        )
    }
}
