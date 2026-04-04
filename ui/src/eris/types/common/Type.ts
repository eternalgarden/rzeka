export class Type {
    name: string
    namespace: string

    constructor(name: string, namespace: string) {
        this.name = name
        this.namespace = namespace
    }

    isEqual(type: Type): boolean {
        return type.namespace == this.namespace && type.name == this.name
    }
}
