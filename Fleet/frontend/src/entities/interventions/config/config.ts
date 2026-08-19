import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

// Relative to the axios baseURL, and must equal the server's [Route(...)] exactly — repeating the base here
// sends requests to /api/api/... See entities.setup → The URL contract.
const api = "/interventions"

const config: IConfig = {
    id: Entity.name,
    key: "Intervention",
    isComplex: true, // relations + an owned collection -> Details page

    routePrefix: "interventions", // TODO: URL path segment
    baseQueryParams: {}, // add includes ONLY for a COLLECTION the API gates behind its named [Flags] enum, e.g. { includes: ["Lines"] }; a to-one shown on every row belongs in the API's unconditional e.Includes instead
    initialQuery: {}, // route query for the GENERATED nav link ONLY — lost on refresh/deep-link. A default sortBy or includes belongs in baseQueryParams

    overviewTitle: "interventions", // camelCase i18n keys (multi-word → e.g. shoppingLists / shoppingList) — add matching entries to public/data/translations.json, or the nav renders the raw key
    detailsTitle: "intervention",
    description: "intervention.description",
    icon: "bi bi-tools",

    defaultPageSize: 20,

    api, // every *Url below defaults to `api` when omitted; keep only the ones you override
    searchUrl: api + "/search", // counted search endpoint — the overview pages through it (every controller exposes /search)
    saveUrl: api, // resource base — update/remove append /{$id} themselves
}

export default config
