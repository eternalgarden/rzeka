import type { CSSDirective, ViewTemplate } from "@microsoft/fast-element"

import {
    FASTElement,
    customElement,
    html,
    css,
    attr,
    ref,
    when,
    ElementStyles,
} from "@microsoft/fast-element"

// using templates
// https://www.fast.design/docs/fast-element/declaring-templates
// "First, we create a template by using a tagged template literal.
// The tag, html, provides special processing for the HTML
// string that follows, returning an instance of ViewTemplate."
const template: ViewTemplate<FloatyBox> = html`
    <div
        class="container"
        part="container"
        ${ref("container")}>
        ${when(
            x => x.draggable,
            html<FloatyBox>`
                <slot
                    name="handlebar"
                    part="handlebar"
                    ${ref("containerHeader")}>
                    <div class="defaultHandlebar">${x => x.emoji}</div>
                </slot>
            `
        )}
        <div
            class="content"
            part="content">
            <slot>Content goes here</slot>
        </div>
    </div>
`

// width: 330px; // ! this must be 30px longer on chromium to hide the scrollbar than the parent
const styles = css`
    div.container {
        color: #35353c;
        border: dotted black;
        margin: 3px;
        overflow: hidden;
    }

    div.content {
        padding: 0.2em;
        position: relative;
        scroll-behavior: smooth;
        max-height: 85vh;
    }

    div.defaultHandlebar {
        min-width: 60px;
        text-align: right;
        background-color: rgba(67, 72, 70, 0);
        padding: 0.2em;
    }

    div::-webkit-scrollbar {
        width: 0;
    }

    div::-webkit-scrollbar-button {
        width: 0;
    }

    div::-webkit-scrollbar-track {
        width: 0;
    }

    div:::-webkit-scrollbar-track-piece {
        width: 0;
    }

    div::-webkit-scrollbar-thumb {
        width: 0;
    }

    div::-webkit-scrollbar-corner {
        width: 0;
    }

    div::-webkit-resizer {
        width: 0;
    }
`

export class FloatyBox extends FASTElement {
    @attr padding: number = 0
    @attr top: string = ""
    @attr left: string = ""
    @attr color: string = "cyan"
    @attr emoji: string = "💎"
    @attr({ mode: "boolean" }) draggable: boolean = false
    container!: HTMLElement
    containerHeader!: HTMLElement

    // * IMPORTANT
    // https://www.fast.design/docs/fast-element/leveraging-css#styles-and-the-element-lifecycle
    resolveStyles() {
        let style = styles

        style = this.extendStyle(
            style,
            css.partial`div.container { background-color: ${this.color};}`
        )

        if (this.draggable) {
            style = this.extendStyle(
                style,
                css.partial`div.container { position: absolute; }`
            )

            if (this.top.length > 0) {
                style = this.extendStyle(
                    style,
                    css.partial`div.container { top: ${this.top} }`
                )
            }

            if (this.left.length > 0) {
                style = this.extendStyle(
                    style,
                    css.partial`div.container { left: ${this.left} }`
                )
            }
        }

        return style
    }

    connectedCallback() {
        super.connectedCallback()

        if (this.draggable) {
            this.dragElement(this.containerHeader, this.container)
        }
    }

    extendStyle(style: ElementStyles, inline: CSSDirective) {
        const extension = css.partial`${inline}`
        return css`
            ${style} ${extension}
        `
    }

    dragElement(dragHandle: HTMLElement, element: HTMLElement) {
        // inspired by https://www.w3schools.com/howto/howto_js_draggable.asp

        let newX = 0,
            newY = 0,
            offsetX = 0,
            offsetY = 0

        dragHandle.onmousedown = dragMouseDown

        function dragMouseDown(e: MouseEvent) {
            e = e || window.event
            e.preventDefault()

            offsetX = e.clientX - element.offsetLeft
            offsetY = e.clientY - element.offsetTop

            document.onmouseup = closeDragElement
            document.onmousemove = elementDrag
        }

        function elementDrag(e: MouseEvent) {
            e = e || window.event
            e.preventDefault()

            newX = e.clientX - offsetX
            newY = e.clientY - offsetY

            element.style.top = newY + "px"
            element.style.left = newX + "px"
        }

        function closeDragElement() {
            document.onmouseup = null
            document.onmousemove = null
        }
    }
}

FloatyBox.define({
    name: "sun-container",
    template,
    styles
})