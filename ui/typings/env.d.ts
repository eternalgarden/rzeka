/// <reference types="vite/client" />

declare module "*.json" {
    const content: any
    export default content
}
