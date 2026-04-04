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

    ::part(button) {
        padding: 2px;
        height: auto;
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

const template = html<MessageItem>`
    <sanctuary-foldout
        class="message-item ${x => x.messageTypeClass} ${x => x.filterClass}"
        tabindex="-1">
        <div
            slot="start"
            class="emojiguid">
            <span>${x => selectItemEmoji(x.occurence.messageType)}</span>
        </div>
        <span slot="title">
            <div class="heading">
                <span>${x => getMessage(x.occurence)}</span>
            </div>
        </span>
        <span slot="collapsed-icon">-</span>
        <span slot="expanded-icon">✨</span>
        <div class="region-content">
            ${when(
                x => x.occurence.messageType == MessageType.Horror,
                html<MessageItem>`
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

        console.log(getMessage(this.occurence))
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

function getMessage(occurence: IRawMessageOccurence): string {
    let message = ""

    const messageType = occurence.messageType

    message = occurence.message

    // if (messageType == MessageType.Hint) message = occurence.message
    // if (messageType == MessageType.Hunch) message = occurence.message
    // if (messageType == MessageType.Horror)
    //     message =
    //         occurence.exception != null
    //             ? occurence.exception.message
    //             : "EXCEPTION NULL"

    console.log(message)

    return message
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
