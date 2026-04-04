import { IRawOccurence } from "../IRawOccurence"
import { MessageException } from "../../common/message/MessageException"
import { MessageType } from "../../common/message/MessageTypeEnum"

export interface IRawMessageOccurence extends IRawOccurence {
    message: string
    messageType: MessageType
    exception: MessageException // 🔥🛠️ | undefined
}
