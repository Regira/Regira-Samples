import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

// Relative to the axios baseURL, and must equal the server's [Route(...)] exactly — repeating the base here
// sends requests to /api/api/... See entities.setup → The URL contract.
const api = "/locations"

const config: IConfig = {
    id: Entity.name,
    key: "Location",
    isComplex: false,
    //                  fields, no relations/tabs — that is fine to edit in a modal (FormModalButton). See entities.card → page vs modal.

    routePrefix: "locations",
    baseQueryParams: {},
    initialQuery: {}, // route query for the GENERATED nav link ONLY — lost on refresh/deep-link. A default sortBy or includes belongs in baseQueryParams

    overviewTitle: "locations",
    detailsTitle: "location",
    description: "locationDescription",
    icon: "bi bi-geo-alt",

    defaultPageSize: 25,

    api, // every *Url below defaults to `api` when omitted; keep only the ones you override
    searchUrl: api + "/search", // counted search endpoint — the overview pages through it (every controller exposes /search)
    saveUrl: api, // resource base — update/remove append /{$id} themselves
}

export default config
