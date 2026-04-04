import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"

export interface IArchivedOccurence {
    guid: string
    occurenceCategory: OccurenceCategory
    timestamp: number
}
