import { SerializableLooming } from "../types/common/spells/SerializableLooming"
import { SerializableStranding } from "../types/common/spells/SerializableStranding"
import { SerializableWeaving } from "../types/common/spells/SerializableWeaving"
import { ISerializableStrandingSpell } from "../types/common/spells/ISerializableStrandingSpell"
import { ISerializableSpell } from "../types/common/spells/ISerializableSpell"
import { Type } from "../types/common/Type"
import { Who } from "../types/common/Who"
import { Dictionary } from "../types/common/Dictionary"
import { SpellSchool } from "../types/common/spells/SpellSchoolEnum"

function deserializeIngredients(xoxo: any): Dictionary<Boolean> {
    const ingredients = new Dictionary<Boolean>()

    Object.keys(xoxo).forEach(key => {
        ingredients.data[key] = xoxo[key]
    })

    return ingredients
}

export function deserializeSpell(spellData: any): ISerializableSpell {
    let deserializedSpell: ISerializableSpell

    const { spellSchool, Who }: { spellSchool: SpellSchool; Who: Who } =
        spellData

    const { guid, title, wasCast, whosName } = spellData

    if (spellSchool == SpellSchool.Stranding) {
        const { conjuredType }: { conjuredType: Type } = spellData

        const strandingSpell: SerializableStranding = {
            conjuredType,
            spellSchool: SpellSchool.Stranding,
            guid,
            title,
            whosName,
            wasCast: false,
            Who,
        }

        deserializedSpell = strandingSpell
    } else if (spellSchool == SpellSchool.Weaving) {
        const {
            hasMana,
            ingredients: xoxo,
        }: { hasMana: boolean; ingredients: any } = spellData

        const ingredients: Dictionary<Boolean> = deserializeIngredients(xoxo)

        // console.log(ingredients);
        const weavingSpell: SerializableWeaving = {
            hasMana,
            ingredients,
            spellSchool: SpellSchool.Weaving,
            guid,
            title,
            whosName,
            wasCast: false,
            Who,
        }

        deserializedSpell = weavingSpell
    } else if (spellSchool == SpellSchool.Looming) {
        const { conjuredType }: { conjuredType: Type } = spellData
        const {
            hasMana,
            ingredients: xoxo,
        }: { hasMana: boolean; ingredients: any } = spellData

        const ingredients: Dictionary<Boolean> = deserializeIngredients(xoxo)

        const loomingSpell: SerializableLooming = {
            hasMana,
            ingredients,
            conjuredType,
            spellSchool: SpellSchool.Looming,
            guid,
            title,
            whosName,
            wasCast: false,
            Who,
        }

        deserializedSpell = loomingSpell
    } else {
        throw new Error("ERRER wait whatttt!!!!!!!!!!!!!!!!!!")
    }

    if (deserializeSpell === null) throw new Error("ERRER NULL SPELL")

    const baseSpell: ISerializableSpell =
        deserializedSpell as ISerializableSpell

    baseSpell.guid = guid
    baseSpell.title = title
    baseSpell.wasCast = wasCast
    baseSpell.whosName = whosName

    return baseSpell
}
