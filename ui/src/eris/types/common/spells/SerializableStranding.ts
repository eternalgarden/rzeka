import { SpellSchool } from "./SpellSchoolEnum"
import { Type } from "../Type"
import { Who } from "../Who"
import { ISerializableStrandingSpell } from "./ISerializableStrandingSpell"

export class SerializableStranding implements ISerializableStrandingSpell {
    Who: Who
    conjuredType: Type
    guid: string
    title: string
    spellSchool: SpellSchool
    whosName: string
    wasCast: boolean
}
