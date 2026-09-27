/** The picked category is the first id of the (expanded) CategoryId filter; route values arrive as strings. */
export function pickedCategory(value: unknown): number | undefined {
    const first = Array.isArray(value) ? value[0] : value
    const id = Number(first)
    return first != null && first !== "" && !Number.isNaN(id) ? id : undefined
}

/** A boolean filter as the route hands it back: true / false / "true" / "false" / undefined. */
export function parseBool(value: unknown): boolean | undefined {
    if (value === true || value === "true") return true
    if (value === false || value === "false") return false
    return undefined
}
