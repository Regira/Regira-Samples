import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

// Relative to the axios baseURL, and must equal the server's [Route(...)] exactly — repeating the base here
// sends requests to /api/api/... See entities.setup → The URL contract.
const api = "/shopping-lists"

const config: IConfig = {
    id: Entity.name,
    key: "ShoppingList",
    isComplex: true,

    routePrefix: "shopping-lists",
    baseQueryParams: {}, // add includes ONLY for a COLLECTION the API gates behind its named [Flags] enum, e.g. { includes: ["Lines"] }; a to-one shown on every row belongs in the API's unconditional e.Includes instead
    initialQuery: {}, // route query for the GENERATED nav link ONLY — lost on refresh/deep-link. A default sortBy or includes belongs in baseQueryParams

    overviewTitle: "shoppingLists",
    detailsTitle: "shoppingList",
    description: "shoppingList.description",
    icon: "bi bi-basket2",

    defaultPageSize: 20,

    api, // every *Url below defaults to `api` when omitted; keep only the ones you override
    searchUrl: api + "/search", // counted search endpoint — the overview pages through it (every controller exposes /search)
    saveUrl: api, // resource base — update/remove append /{$id} themselves
}

export default config
