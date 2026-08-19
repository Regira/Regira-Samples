import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

const api = "/support-teams"

const config: IConfig = {
    id: Entity.name,
    key: "SupportTeam",
    isComplex: true,

    routePrefix: "support-teams",
    baseQueryParams: {}, // MemberCount is populated server-side by a processor, independent of ?includes=
    initialQuery: {},

    overviewTitle: "supportTeams",
    detailsTitle: "supportTeam",
    description: "supportTeam.description",
    icon: "bi bi-people",

    defaultPageSize: 15,

    api,
    searchUrl: api + "/search",
    saveUrl: api,
}

export default config
