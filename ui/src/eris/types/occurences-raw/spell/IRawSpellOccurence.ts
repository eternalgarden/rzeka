import { SpellOccurenceCategory } from "../../occurence-categories/SpellOccurenceCategory"
import { IRawOccurence } from "../IRawOccurence"

export interface IRawSpellOccurence extends IRawOccurence {
    spellOccurenceCategory: SpellOccurenceCategory
}
