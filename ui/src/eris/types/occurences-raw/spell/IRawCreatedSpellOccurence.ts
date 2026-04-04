import { ISerializableSpell } from "../../common/spells/ISerializableSpell"
import { IRawSpellOccurence } from "./IRawSpellOccurence"

export interface IRawCreatedSpellOccurence extends IRawSpellOccurence {
    spell: ISerializableSpell
}
