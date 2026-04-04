import { MessageException } from "../types/common/message/MessageException"
import { IRawMessageOccurence } from "../types/occurences-raw/message/IRawMessageOccurence"

export function deserializeMessageOccurence(json: any): IRawMessageOccurence {
    const {
        guid,
        occurenceCategory,
        messageType,
        circumstances,
        spell,
        message,
        timestamp,
        exception,
    } = json

    const { message: exceptionMessage, stackTrace, comments } = exception

    let deserializedException: MessageException = {
        message: exceptionMessage,
        stackTrace,
        comments,
    }

    type NewType = IRawMessageOccurence

    const messageOccurence: NewType = {
        guid,
        occurenceCategory,
        messageType,
        message,
        timestamp,
        exception: deserializedException,
        containsText: text => containsText(messageOccurence, text),
    }

    return messageOccurence
}

function containsText(
    messageOccurence: IRawMessageOccurence,
    text: string
): boolean {
    return messageOccurence.message.includes(text.toLowerCase())
}
