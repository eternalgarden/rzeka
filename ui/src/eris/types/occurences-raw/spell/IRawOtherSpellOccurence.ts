import { IRawSpellOccurence } from "./IRawSpellOccurence"

export interface IRawOtherSpellOccurence extends IRawSpellOccurence {
    spellReference: string
}
