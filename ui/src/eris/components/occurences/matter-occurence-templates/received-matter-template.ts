import { html, repeat, when } from "@microsoft/fast-element"
import { MatterItem } from "../MatterItem"
import { MatterOccurenceCategory } from "../../../types/occurence-categories/MatterOccurenceCategory"
import { rootMatterSeparatorTemplate } from "./root-separator-template"
import * as moment from "moment"
import {
    getFewGuidCharacters,
    getFullMatterType,
    getStringFilterMatchClass,
} from "./common-occurence"

const EMOJI: string = "🎯"

export const ReceivedMatterOccurenceTemplate = html<MatterItem>`<sanctuary-foldout
    ?hidden="${x => !x.isVisible}"
    id="root"
    class="
            matter-item
            received-occurence
            ${x => getStringFilterMatchClass(x.containsSearchedText)}"
    tabindex="-1">
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
        class="flex flex-row">
        <div class="flex flex-col flex-grow-one align-items-baseline">
            <span class="self-baseline">${x => x.matterType.name}</span>
        </div>
        <div class="self-center received-count-display">
            <span>${x => x.listOfReceivingSpells.length}</span>
        </div>
    </div>
    <span slot="collapsed-icon">-</span>
    <span slot="expanded-icon">🛸</span>
    <div slot="content" class="region-content">
        <div>
            <p class="label">Received by:</p>
            <ul>
                ${repeat(
                    x => x.listOfReceivingSpells,
                    html<string, MatterItem>`
                        ${(c, x) => {
                            const spellGuid = c
                            const spellTitle =
                                x.parent.spellArchive.getSpellTitle(spellGuid)

                            const spellCasterName =
                                x.parent.spellArchive.getSpellCasterName(
                                    spellGuid
                                )

                            return html` <li class="tight">
                                ${spellCasterName}
                                <ul>
                                    <li>${spellTitle}</li>
                                </ul>
                            </li>`
                        }}
                    `,
                    { positioning: true }
                )}
            </ul>
            <p class="label">Matter information:</p>
            <ul>
                <li>${x => getFullMatterType(x.matterType)}</li>
                <li>${x => x.matterData.guid}</li>
            </ul>
        </div>
        <span class="details">${x => x.occurence.matterGuid}</span>
        <span class="details">
            ${x => moment.unix(x.timestamp).format("MMMM Do YYYY, h:mm:ss a")}
        </span>
    </div>
</sanctuary-foldout>`
