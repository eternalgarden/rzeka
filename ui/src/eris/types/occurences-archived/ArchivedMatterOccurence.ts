import { BehaviorSubject, Observable } from "rxjs"
import { MatterOccurenceCategory } from "../occurence-categories/MatterOccurenceCategory"
import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"
import { IArchivedMatterOccurence } from "./IArchivedMatterOccurence"

export class ArchivedMatterOccurence implements IArchivedMatterOccurence {
    // 🌲 IArchivedOccurence
    readonly guid: string
    readonly timestamp: number
    readonly occurenceCategory: OccurenceCategory = OccurenceCategory.Matter

    // 🌲 IArchivedMatterOccurence
    readonly matterOccurenceCategory: MatterOccurenceCategory
    readonly matterGuid: string

    constructor(
        guid: string,
        timestamp: number,
        matterOccurenceCategory: MatterOccurenceCategory,
        matterGuid: string
    ) {
        this.guid = guid
        this.matterGuid = matterGuid
        this.timestamp = timestamp
        this.matterOccurenceCategory = matterOccurenceCategory
    }
}

export class ArchivedShapedMatterOccurence extends ArchivedMatterOccurence {
    readonly spellGuid: string

    constructor(
        guid: string,
        timestamp: number,
        matterOccurenceCategory: MatterOccurenceCategory,
        matterGuid: string,
        spellGuid: string
    ) {
        super(guid, timestamp, matterOccurenceCategory, matterGuid)

        this.spellGuid = spellGuid
    }
}

export class ArchivedReceivedMatterOccurence extends ArchivedMatterOccurence {

    private listOfReceivingSpells: string[]
    private newReceiverSubject = new BehaviorSubject<string[]>([])
    
    readonly newReceiverObservable: Observable<string[]> =
        this.newReceiverSubject.asObservable()

    constructor(
        guid: string,
        timestamp: number,
        matterOccurenceCategory: MatterOccurenceCategory,
        matterGuid: string,
        spellGuid: string
    ) {
        super(guid, timestamp, matterOccurenceCategory, matterGuid)

        this.listOfReceivingSpells = []
        this.addReceivingSpell(spellGuid)
    }

    addReceivingSpell(spellGuid: string) {
        this.listOfReceivingSpells.push(spellGuid)
        this.newReceiverSubject.next(this.listOfReceivingSpells)
    }
}
