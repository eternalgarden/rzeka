import { html, when } from "@microsoft/fast-element"
import { MatterItem } from "../MatterItem"
import { MatterOccurenceCategory } from "../../../types/occurence-categories/MatterOccurenceCategory"
import { rootMatterSeparatorTemplate } from "./root-separator-template"
import moment from "moment"
import {
    getFewGuidCharacters,
    getFullMatterType,
    getObjectContents,
    getStringFilterMatchClass,
} from "./common-occurence"
import { ArchivedShapedMatterOccurence } from "../../../types/occurences-archived/ArchivedMatterOccurence"
import { IArchivedMatterOccurence } from "../../../types/occurences-archived/IArchivedMatterOccurence"

const EMOJI: string = "🪴"

export const ShapedMatterOccurenceTemplate = html<MatterItem>`<div
        class="matter-row"
        ?hidden="${x => !x.isVisible}">
    <button
        class="select-pin"
        title="Pin as causality tree focus"
        @click="${(x, c) => x.selectThisMatter(c.event)}">📍</button>
    <sanctuary-foldout
        class="
            matter-item
            shaped-occurence
            flex-grow-one
            ${x => getStringFilterMatchClass(x.containsSearchedText)}"
        tabindex="-1">
        <!-- /* 🔥 what is this for? */ -->
        <div
            slot="start"
            class="emojiguid">
            <span>${EMOJI}</span>
            <span class="who">
                ${x => {
                    const guid = x.occurence.matterGuid
                    return getFewGuidCharacters(guid).toUpperCase()
                }}
            </span>
        </div>
        <div
            slot="title"
            class="flex flex-col flex-grow align-items-baseline">
            <span>${x => x.matterType.name}</span>
            <span class="who"
                >${x =>
                    x.spellArchive.getSpellCasterName(
                        getShapedSpellGuid(x.occurence)
                    )}</span
            >
        </div>
        <span slot="collapsed-icon">-</span>
        <span slot="expanded-icon">📜</span>
        <div slot="content" class="region-content">
            <p class="label">Matter Data:</p>
            <div class="pls">
                <ul
                    :innerHTML="${x =>
                        getObjectContents(x.matterContent, "", 0)}"></ul>
            </div>

            <div class="pls">
                <p class="who">Spell information:</p>
                <ul>
                    <li>${x => getFullMatterType(x.matterType)}</li>
                    <li>
                        ${x =>
                            x.spellArchive.getSpellTitle(
                                getShapedSpellGuid(x.occurence)
                            )}
                    </li>
                    <li>${x => getShapedSpellGuid(x.occurence)}</li>
                </ul>
            </div>
            <span class="details">${x => x.occurence.matterGuid}</span>
            <span class="details">
                ${x =>
                    moment.unix(x.timestamp).format("MMMM Do YYYY, h:mm:ss a")}
            </span>
        </div>
    </sanctuary-foldout>
    </div>

    <!-- 🌄 ROOT MATTER SEPARATOR -->
    ${when(
        x => x.hasShapedMatterCircumstances(),
        rootMatterSeparatorTemplate
    )} `

function getShapedSpellGuid(occurence: IArchivedMatterOccurence) {
    return (occurence as ArchivedShapedMatterOccurence).spellGuid
}
