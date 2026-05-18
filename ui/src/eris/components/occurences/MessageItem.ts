import { repeat } from "@microsoft/fast-element"
import {
    FASTElement,
    css,
    customElement,
    html,
    observable,
    when,
} from "@microsoft/fast-element"
import moment from "moment"
import { GefildeDesVorkommen } from "../GefildeDesVorkommen"
import { Subscription, filter } from "rxjs"
import { IRawMessageOccurence } from "../../types/occurences-raw/message/IRawMessageOccurence"
import { MessageType } from "../../types/common/message/MessageTypeEnum"

import matterStyles from "./styles/matter.css?raw"

const messageTypeStyles = css`
    .Hint {
        background-color: rgb(93, 118, 255);
    }

    .Hunch {
        background-color: yellow;
    }

    .Horror {
        background-color: #ff4d6c;
    }
`

const styles = css`

    ${matterStyles}

    .message-item {
        border-bottom: calc(var(--stroke-width) * 1px) solid #000000;
        color: black;
    }

    ::part(toggle-button) {
        padding: 2px;
        height: auto;
        min-height: 44px;
        align-items: flex-start;
    }

    ::part(heading):hover {
        backdrop-filter: contrast(40%);
    }

    .region-content {
        display: flex;
        flex-direction: column;
        row-gap: 2px;
        align-items: left;
    }

    .emojiguid {
        display: flex;
        flex-direction: column;
        align-items: center;
        padding-left: 0.8rem;
    }

    .details {
        align-self: flex-end;
        font-size: 0.9em;
    }

    .heading {
        display: flex;
        flex-direction: column;
        justify-items: center;
        color: #ffffff;
        text-align: left;
        min-width: 0;
    }

    .message-title {
        display: block;
        white-space: normal;
        word-break: break-word;
    }

    .full-message {
        white-space: pre-wrap;
        word-break: break-word;
        margin: 0;
    }

    pre.stack-trace {
        white-space: pre-wrap;
        word-break: break-word;
        margin: 0;
        font-family: inherit;
        font-size: 0.85em;
        opacity: 0.85;
    }

    .who {
        // color: antiquewhite;
        font-size: 0.7rem;
    }

    ul {
        list-style-type: hiragana;
        margin: 0.2em 0;
        // color: #ffe300;
    }

    .filterMatch {
        background-color: red;
    }

    ${messageTypeStyles}
`

const TITLE_MAX_CHARS = 100

const template = html<MessageItem>`
    <sanctuary-foldout
        class="message-item ${x => x.messageTypeClass} ${x => x.filterClass}"
        tabindex="-1">
        <div
            slot="start"
            class="emojiguid">
            <span>${x => selectItemEmoji(x.occurence.messageType)}</span>
        </div>
        <div slot="title" class="heading">
            <span class="message-title">${x => truncateForTitle(x.occurence.message)}</span>
        </div>
        <span slot="collapsed-icon">-</span>
        <span slot="expanded-icon">✨</span>
        <div slot="content" class="region-content">
            <p class="full-message">${x => x.occurence.message}</p>
            ${when(
                x => x.occurence.messageType == MessageType.Horror && hasExceptionMessage(x.occurence),
                html<MessageItem>`
                    <p class="label">Exception:</p>
                    <p class="full-message">${x => x.occurence.exception.message}</p>
                `
            )}
            ${when(
                x => x.occurence.messageType == MessageType.Horror && hasStackTrace(x.occurence),
                html<MessageItem>`
                    <p class="label">Stack trace:</p>
                    <pre class="stack-trace">${x => x.occurence.exception.stackTrace}</pre>
                `
            )}
            ${when(
                x => x.occurence.messageType == MessageType.Horror && hasComments(x.occurence),
                html<MessageItem>`
                    <p class="label">Notes:</p>
                    <ul>
                        ${repeat(
                            x => x.occurence.exception.comments,
                            html<string>`<li>${x => x}</li>`
                        )}
                    </ul>
                `
            )}
            <span class="details">
                ${x =>
                    moment
                        .unix(x.occurence.timestamp)
                        .format("MMMM Do YYYY, h:mm:ss a")}
            </span>
        </div>
    </sanctuary-foldout>
`

@customElement({
    name: "message-item",
    template,
    styles,
})
export class MessageItem extends FASTElement {
    @observable filterClass: string = ""
    @observable messageTypeClass: string = ""
    @observable container: GefildeDesVorkommen
    @observable occurence: IRawMessageOccurence

    filterSubscription: Subscription

    connectedCallback(): void {
        super.connectedCallback()

        this.messageTypeClass = this.occurence.messageType
    }

    containerChanged(_: GefildeDesVorkommen, container: GefildeDesVorkommen) {
        if (this.filterSubscription != undefined)
            this.filterSubscription.unsubscribe()

        // TODO rework like matter item
        // this.filterSubscription = container.searchbarFilter.subscribe(val => {
        //     if (val.length < 3) this.filterClass = ""
        //     else {
        //         if (this.occurence.containsText(val))
        //             this.filterClass = "filterMatch"
        //         else this.filterClass = ""
        //     }
        // })
    }
}

function selectItemEmoji(messageType: MessageType): string {
    let emoji = ""

    if (messageType == MessageType.Hint) emoji += "📮"
    if (messageType == MessageType.Hunch) emoji += "⚠️"
    if (messageType == MessageType.Horror) emoji += "👻"

    return emoji
}

function truncateForTitle(message: string | undefined): string {
    const full = message ?? ""
    if (full.length <= TITLE_MAX_CHARS) return full
    return full.slice(0, TITLE_MAX_CHARS).trimEnd() + "..."
}

function hasExceptionMessage(occurence: IRawMessageOccurence): boolean {
    return !!occurence.exception && !!occurence.exception.message
}

function hasStackTrace(occurence: IRawMessageOccurence): boolean {
    return !!occurence.exception && !!occurence.exception.stackTrace
}

function hasComments(occurence: IRawMessageOccurence): boolean {
    return !!occurence.exception
        && Array.isArray(occurence.exception.comments)
        && occurence.exception.comments.length > 0
}

// function getContent(occurence: MessageOccurence): string {
//     let content = ""

//     const messageType = occurence.messageType

//     if (messageType == MessageType.Horror)
//         message =
//             occurence.exception != null
//                 ? occurence.exception.message
//                 : "EXCEPTION NULL"

//     console.log(content)

//     return content
// }
