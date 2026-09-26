import { shallowRef } from "vue"
import { parseCsv } from "@/infrastructure/csv"

export { parseCsv }

// Every text on the game screens lives in public/data/screen-texts.csv (Screen;Group;Text;Detail).
// A Group is a pool: when it holds several lines, a random one is picked, so adding a row adds variety
// without touching code. Fixed labels are simply groups with a single line. Texts may carry {placeholders}.

export interface ScreenText {
    screen: string
    group: string
    text: string
    /** extra column: the author of a testimonial, the HTTP code of an error text */
    detail?: string
}

export type TextValues = Record<string, string | number | undefined>

const texts = shallowRef<Array<ScreenText>>([])
const reported = new Set<string>()

/** Replaces {name} placeholders; unknown placeholders are left as they are. */
export function fill(text: string, values?: TextValues) {
    return values ? text.replace(/\{(\w+)\}/g, (match, key: string) => (values[key] != null ? String(values[key]) : match)) : text
}

/** Reads the CSV once at startup. Without it the screens show their group names (and the console says why). */
export async function loadScreenTexts(url: string) {
    try {
        const response = await fetch(url, { cache: "no-cache" }) // revalidate: a redeploy's texts show up without a hard refresh
        if (!response.ok) throw new Error(`${response.status} ${response.statusText}`)
        const [header, ...lines] = parseCsv(await response.text())
        const col = (name: string) => header!.findIndex((h) => h.trim().toLowerCase() === name)
        const [screen, group, text, detail] = ["screen", "group", "text", "detail"].map(col)
        texts.value = lines
            .map((l) => ({ screen: l[screen!] ?? "", group: l[group!] ?? "", text: l[text!] ?? "", detail: l[detail!] || undefined }))
            .filter((t) => t.group && t.text)
    } catch (ex) {
        console.warn(`Screen texts could not be loaded from ${url}.`, ex)
    }
}

export function useScreenTexts() {
    const rows = (group: string, detail?: string | number) =>
        texts.value.filter((t) => t.group === group && (detail == null || t.detail === String(detail)))
    /** all texts of a group, in file order */
    const all = (group: string, detail?: string | number) => rows(group, detail).map((t) => t.text)
    /** a text of the group filtered on Detail (random when there are several), with {placeholders} filled */
    const tFor = (group: string, detail: string | number | undefined, values?: TextValues) => {
        const pool = all(group, detail)
        if (!pool.length) {
            const id = detail == null ? group : `${group}/${detail}`
            if (!reported.has(id)) {
                reported.add(id)
                console.warn(`screen-texts.csv has no row for group "${id}"`)
            }
            return group
        }
        return fill(pool[Math.floor(Math.random() * pool.length)]!, values)
    }
    /** a text of the group (random when there are several), with {placeholders} filled */
    const t = (group: string, values?: TextValues) => tFor(group, undefined, values)
    return { rows, all, t, tFor }
}
