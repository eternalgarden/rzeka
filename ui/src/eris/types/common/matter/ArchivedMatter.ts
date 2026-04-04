import { BehaviorSubject, Observable } from "rxjs"
import { Type } from "../Type"
import { MatterData } from "./MatterData"

export class ArchivedMatter {
    readonly matterType: Type
    readonly matterData: MatterData
    readonly spellReference: string

    private receivedByCountSubject = new BehaviorSubject<number>(0);
    receivedCountObservable: Observable<number> = this.receivedByCountSubject.asObservable();

    receivedBy: string[]

    constructor(matterType: Type, matter: MatterData, spellReference: string) {
        this.matterType = matterType
        this.matterData = matter
        this.spellReference = spellReference
    }

    addReceivedBy(occurenceGuid: string)
    {
        if (this.receivedBy === undefined) this.receivedBy = []
        this.receivedBy.push(occurenceGuid)
        this.receivedByCountSubject.next(this.receivedBy.length); // 🔥 Push the new count
    }
}
