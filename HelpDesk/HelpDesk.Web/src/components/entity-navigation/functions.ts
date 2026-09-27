import { computed, getCurrentInstance } from "vue"
import { type IConfig, importDashboard, importNavbar, buildNavigationTree } from "@regira/modules/vue/entities"
import { useConfig } from "@/app-config"
import { useAccess } from "@/infrastructure/access"

// reference data is managed by administrators only (mirrors the API's write filter)
const ADMIN_ONLY = ["SupportTeam", "Category", "Priority", "Status"]

// reads config.json → navigation + the collected $configs, builds the dashboard/navbar trees
export function useNavigation() {
    const app = getCurrentInstance()!
    const {
        navigation: { groups, dashboard, navbar, search },
    } = useConfig()

    const configs = Object.values(app.appContext.config.globalProperties.$configs) as Array<IConfig>
    const { isAdmin, isStaff } = useAccess()
    const hasAccess = (config: IConfig) => (ADMIN_ONLY.includes(config.key) ? isAdmin.value : config.key === "Person" ? isStaff.value : true)

    const dashboardTree = computed(() => buildNavigationTree(importDashboard({ groups, entities: dashboard, configs, hasAccess })))
    const navbarTree = computed(() => buildNavigationTree(importNavbar({ groups, entities: navbar, configs, hasAccess })))
    const searchItemConfig = computed(() => configs.find((c) => c.key === search))

    return { dashboardTree, navbarTree, searchItemConfig }
}
