import { MatterOccurenceCategory } from "../../occurence-categories/MatterOccurenceCategory"
import { IRawOccurence } from "../IRawOccurence"

export interface IRawMatterOccurence extends IRawOccurence {
    matterOccurenceCategory: MatterOccurenceCategory // * Self-explanatory
    spellGuid: string // * The spell that emitted this matter occurence
}
