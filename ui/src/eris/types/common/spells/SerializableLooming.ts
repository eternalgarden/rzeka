import { SpellSchool } from "./SpellSchoolEnum"
import { Dictionary } from "../Dictionary"
import { Type } from "../Type"
import { Who } from "../Who"
import { ISerializableStrandingSpell } from "./ISerializableStrandingSpell"
import { ISerlializableBindingSpell } from "./ISerlializableBindingSpell"

export class SerializableLooming
    implements ISerializableStrandingSpell, ISerlializableBindingSpell
{
    Who: Who
    conjuredType: Type
    guid: string
    title: string
    spellSchool: SpellSchool
    whosName: string
    wasCast: boolean
    hasMana: boolean
    ingredients: Dictionary<Boolean>
}
