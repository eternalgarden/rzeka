import { OccurenceCategory } from "../occurence-categories/OccurenceCategory"

export interface IRawOccurence {
    occurenceCategory: OccurenceCategory
    guid: string // TODO occurence need not be that precise, replace with a simple counter on unity side
    timestamp: number
    containsText: (text: string) => boolean // TODO cut that out
}
