import { Dictionary } from "../Dictionary"
import { ISerializableSpell } from "./ISerializableSpell"

export interface ISerlializableBindingSpell extends ISerializableSpell {
    hasMana: boolean
    ingredients: Dictionary<Boolean>
}
