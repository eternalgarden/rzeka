import { SpellSchool } from "./SpellSchoolEnum"
import { Dictionary } from "../Dictionary"
import { Who } from "../Who"
import { ISerlializableBindingSpell } from "./ISerlializableBindingSpell"

export class SerializableWeaving implements ISerlializableBindingSpell {
    Who: Who
    hasMana: boolean
    ingredients: Dictionary<Boolean>
    guid: string
    title: string
    spellSchool: SpellSchool
    whosName: string
    wasCast: boolean
}
