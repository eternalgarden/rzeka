import { IRawMatterOccurence } from "./IRawMatterOccurence"

export interface IRawReceivedMatterOccurence extends IRawMatterOccurence {
    receivedMatterGuid: string
}
