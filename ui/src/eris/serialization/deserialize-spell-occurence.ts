import { ISerializableSpell } from "../types/common/spells/ISerializableSpell"
import { SpellOccurenceCategory } from "../types/occurence-categories/SpellOccurenceCategory"
import { IRawOtherSpellOccurence } from "../types/occurences-raw/spell/IRawOtherSpellOccurence"
import { IRawCreatedSpellOccurence } from "../types/occurences-raw/spell/IRawCreatedSpellOccurence"
import { IRawSpellOccurence } from "../types/occurences-raw/spell/IRawSpellOccurence"
import { deserializeSpell } from "./deserialize-spell"

export function deserializeSpellOccurence(data: any): IRawSpellOccurence {
    const { guid, occurenceCategory, spellOccurenceCategory, timestamp } = data

    if (spellOccurenceCategory == SpellOccurenceCategory.Created) {
        const { spell } = data

        const deserializedSpell = deserializeSpell(spell) as ISerializableSpell

        const spellOccurence: IRawCreatedSpellOccurence = {
            guid,
            occurenceCategory,
            timestamp,
            spellOccurenceCategory,
            spell: deserializedSpell,
            containsText: text => containsText(spellOccurence, text),
        }

        return spellOccurence
    } else {
        const { spellReference } = data

        const spellOccurence: IRawOtherSpellOccurence = {
            guid,
            occurenceCategory,
            timestamp,
            spellOccurenceCategory,
            spellReference,
            containsText: text => false,
        }

        return spellOccurence
    }
}

// TODO GERT RID OF ZIS
// move it to rx
const containsText = (
    occurence: IRawCreatedSpellOccurence,
    text: string
): boolean => {
    const lText = text.toLowerCase()
    const spellTitle = occurence.spell.title.toLowerCase()
    const whosType = occurence.spell.Who.WhosType.name.toLowerCase()
    const doesit = spellTitle.includes(lText) || whosType.includes(lText)
    // console.log(doesit);
    return doesit
}
