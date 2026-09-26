// Shared by the game texts (public/data/screen-texts.csv) and the admin emoji picker (public/data/emoji-picker.csv).

/** Minimal RFC 4180 parser: quoted fields (with "" escapes, delimiters and line breaks inside), CRLF or LF, BOM. */
export function parseCsv(input: string, delimiter = ";"): Array<Array<string>> {
    const rows: Array<Array<string>> = []
    let row: Array<string> = []
    let field = ""
    let quoted = false
    const s = input.replace(/^﻿/, "")
    for (let i = 0; i < s.length; i++) {
        const c = s[i]
        if (quoted) {
            if (c === '"' && s[i + 1] === '"') {
                field += '"'
                i++
            } else if (c === '"') quoted = false
            else field += c
        } else if (c === '"' && field === "") quoted = true
        else if (c === delimiter) {
            row.push(field)
            field = ""
        } else if (c === "\n" || c === "\r") {
            if (c === "\r" && s[i + 1] === "\n") i++
            row.push(field)
            if (row.some((f) => f !== "")) rows.push(row)
            row = []
            field = ""
        } else field += c
    }
    row.push(field)
    if (row.some((f) => f !== "")) rows.push(row)
    return rows
}
