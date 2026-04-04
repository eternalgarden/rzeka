import * as moment from "moment"
import { IRawReceivedMatterOccurence } from "../types/occurences-raw/matter/IRawReceivedMatterOccurence"
import { IRawShapedMatterOccurence } from "../types/occurences-raw/matter/IRawShapedMatterOccurence"
import { IRawMatterOccurence } from "../types/occurences-raw/matter/IRawMatterOccurence"

import { deserializeSpell } from "./deserialize-spell"
import { MatterOccurenceCategory } from "../types/occurence-categories/MatterOccurenceCategory"

export function deserializeMatterOccurence(
    data: any
): IRawMatterOccurence | undefined {
    const {
        guid,
        timestamp,
        occurenceCategory,
        matterOccurenceCategory,
        spellGuid,
    } = data

    const category = matterOccurenceCategory as MatterOccurenceCategory

    if (category == MatterOccurenceCategory.Shaped) {
        const { matter: matterObject, matterType } = data

        const {
            Guid: matterGuid,
            Circumstances,
            Description,
            ...Content
        } = matterObject

        const shapedMatterOccurence: IRawShapedMatterOccurence = {
            guid,
            occurenceCategory,
            matterOccurenceCategory,
            matterType,
            timestamp,
            matter: {
                guid: matterGuid,
                description: Description,
                circumstances: Circumstances,
                content: Content,
            },
            spellGuid,
            containsText: text =>
                matterOccurenceContainsText(shapedMatterOccurence, text),
        }

        return shapedMatterOccurence
    } else if (category == MatterOccurenceCategory.Received) {
        const { receivedMatterGuid } = data

        const receivedMatterOccurence: IRawReceivedMatterOccurence = {
            guid,
            timestamp,
            occurenceCategory,
            matterOccurenceCategory,
            receivedMatterGuid,
            spellGuid,
            containsText: text =>
                matterOccurenceContainsText(receivedMatterOccurence, text),
        }

        return receivedMatterOccurence
    } else {
        console.error(`🔥 DERP. Unhandled Matter Type: ${category}`)
        return undefined
    }
}

const matterOccurenceContainsText = (
    occurence: IRawMatterOccurence,
    text: string
): boolean => {
    const lText = text.toLowerCase()
    return false // TODO
    // return occurence.matterType.name.toLowerCase().includes(lText)
}
