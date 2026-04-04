import { SpellSchool } from "./SpellSchoolEnum"
import { Who } from "../Who"

export interface ISerializableSpell {
    guid: string
    title: string
    spellSchool: SpellSchool
    whosName: string
    wasCast: boolean
    Who: Who
}
