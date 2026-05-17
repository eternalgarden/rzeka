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

        deserializedSpell = {
            conjuredType,
            spellSchool,
            guid,
            title,
            whosName,
            wasCast: false,
            Who,
        } as SerializableStranding
    } else if (spellSchool == SpellSchool.Plucking) {
        // PluckingSpell implements IStrandingSpell on the C# side, so the wire
        // payload is identical to Stranding — only the spellSchool tag differs.
        const { conjuredType }: { conjuredType: Type } = spellData

        deserializedSpell = {
            conjuredType,
            spellSchool,
            guid,
            title,
            whosName,
            wasCast: false,
            Who,
        } as SerializableStranding
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

        deserializedSpell = {
            hasMana,
            ingredients,
            conjuredType,
            spellSchool: SpellSchool.Looming,
            guid,
            title,
            whosName,
            wasCast: false,
            Who,
        } as SerializableLooming
    } else if (spellSchool == SpellSchool.Shuttling) {
        // ShuttleSpell extends LoomingSpell on the C# side, so the wire payload
        // is identical to Looming — only the spellSchool tag differs.
        const { conjuredType }: { conjuredType: Type } = spellData
        const {
            hasMana,
            ingredients: xoxo,
        }: { hasMana: boolean; ingredients: any } = spellData

        const ingredients: Dictionary<Boolean> = deserializeIngredients(xoxo)

        deserializedSpell = {
            hasMana,
            ingredients,
            conjuredType,
            spellSchool: SpellSchool.Shuttling,
            guid,
            title,
            whosName,
            wasCast: false,
            Who,
        } as SerializableLooming
    } else {
        throw new Error(`🧨 Unknown spell school: ${spellSchool}, can't deserialize.`)
    }

    const baseSpell: ISerializableSpell =
        deserializedSpell as ISerializableSpell

    baseSpell.guid = guid
    baseSpell.title = title
    baseSpell.wasCast = wasCast
    baseSpell.whosName = whosName

    return baseSpell
}
