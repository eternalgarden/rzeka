import { attr, css, FASTElement, html, observable } from "@microsoft/fast-element"
import windStyles from "../../styles/wind.css?raw"
import foldoutHtml from "./foldout.html?raw"

// ✨ Actually can load partial htmls too
// const template = html`
//     ${html.partial(foldoutHtml)}
// `

const template = html<Foldout>`
<div 
    part="heading" 
    role="heading">
    <button
        part="toggle-button"
        @click="${(x, c) => x.onToggle(c.event)}"
        class="flex flex-row gap-medium align-items-center">
        <div part="start"><slot name="start"/></div>
        <div part="title"><slot name="title"/></div>
        <div part="icon" class="icon" ?hidden=${(x,c) => !x.isCollapsed}><slot name="collapsed-icon"/>-</div>
        <div part="icon" class="icon" ?hidden=${(x,c) => x.isCollapsed}><slot name="expanded-icon"/>✨</div>
    </button>
</div>
<div 
    ?hidden=${(x,c) => x.isCollapsed} 
    class="content" 
    part="content" 
    role="region">
    <slot name="content"></slot>
</div>
`

/**
 * Create CSS styles using the css tag template literal
 */
const styles = css`
    ${windStyles}

    :host {
        display: block; /* Ensures that the foldout component behaves like a block element */
    }

    button {
        border: none;
        background-color: transparent;
        color: white;
        box-sizing: border-box;
        padding: 0;
        height: 44px;
        width:100%;
        font-family: inherit;
    }

    div.icon {
        min-width: 40px;
    }
`

/**
 * Define your component logic by creating a class that extends
 * the FASTElement, note the addition of the attr decorator,
 * this creates an attribute on your component which can be passed.
 */
class Foldout extends FASTElement {
    @attr name: string

    @attr isCollapsed: boolean = true

    onToggle(x: any) {
        this.isCollapsed = !this.isCollapsed
    }
}

/**
 * Define your custom web component for the browser, as soon as the file
 * containing this logic is imported, the element "hello-world" will be
 * defined in the DOM with it's html, styles, logic, and tag name.
 */
Foldout.define({
    name: "sanctuary-foldout",
    template,
    styles,
})
