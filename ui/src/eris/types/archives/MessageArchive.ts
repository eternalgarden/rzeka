import { Observable, filter, map, tap } from "rxjs"
import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"
import { IRawOccurence } from "../occurences-raw/IRawOccurence"
import { IRawMessageOccurence } from "../occurences-raw/message/IRawMessageOccurence"
import { ArchivedMessageOccurence } from "../occurences-archived/ArchivedMessageOccurence"

export class MessageArchive {
    readonly newMessageOccurenceProcessed: Observable<ArchivedMessageOccurence>

    constructor(newOccurenceObservable: Observable<IRawOccurence>) {
        this.newMessageOccurenceProcessed = newOccurenceObservable.pipe(
            filter(occ => occ.occurenceCategory === OccurenceCategory.Message),
            map(occ => occ as IRawMessageOccurence),
            map(
                occ =>
                    new ArchivedMessageOccurence(
                        occ.guid,
                        occ.timestamp,
                        occ.messageType,
                        occ.message,
                        occ.exception
                    )
            )
            // tap(occ => console.log("msg"))
        )
    }
}
