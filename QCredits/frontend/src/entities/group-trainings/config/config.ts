import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

const api = "/group-trainings"

const config: IConfig = {
    id: Entity.name,
    key: "GroupTraining",
    isComplex: true,

    routePrefix: "group-trainings",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "groupTrainings",
    detailsTitle: "groupTraining",
    description: "groupTraining.description",
    icon: "bi bi-people-fill",

    defaultPageSize: 12,

    api,
    searchUrl: api + "/search",
    saveUrl: api
}

export default config
