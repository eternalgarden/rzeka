import { MatterOccurenceCategory } from "../occurence-categories/MatterOccurenceCategory"
import { IArchivedOccurence } from "./IArchivedOccurence"

export interface IArchivedMatterOccurence extends IArchivedOccurence {
    matterOccurenceCategory: MatterOccurenceCategory
    matterGuid: string
}