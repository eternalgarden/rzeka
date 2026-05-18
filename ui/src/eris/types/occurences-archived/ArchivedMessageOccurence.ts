import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"
import { IArchivedOccurence } from "./IArchivedOccurence"
import { MessageException } from "../common/message/MessageException"
import { MessageType } from "../common/message/MessageTypeEnum"

export class ArchivedMessageOccurence implements IArchivedOccurence {
    readonly occurenceCategory: OccurenceCategory = OccurenceCategory.Message
    readonly guid: string
    readonly messageType: MessageType
    readonly message: string
    readonly exception: MessageException | null
    readonly timestamp: number

    constructor(
        guid: string,
        timestamp: number,
        messageType: MessageType,
        message: string,
        exception: MessageException | null
    ) {
        this.guid = guid
        this.timestamp = timestamp
        this.messageType = messageType
        this.message = message
        this.exception = exception
    }
}
