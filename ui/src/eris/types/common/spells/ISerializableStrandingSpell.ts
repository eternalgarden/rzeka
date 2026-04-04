import { Type } from "../Type"
import { ISerializableSpell } from "./ISerializableSpell"

export interface ISerializableStrandingSpell extends ISerializableSpell {
    conjuredType: Type
}
