ACCOUNT_INFRA   := CoffeePeek.Account.Infrastructure/CoffeePeek.Account.Infrastructure.csproj
ACCOUNT_STARTUP := CoffeePeek.AccountService/CoffeePeek.AccountService.csproj
ACCOUNT_CONTEXT := CoffeePeek.Auth.Infrastructure.Configuration.AccountDbContext

SHOPS_INFRA     := CoffeePeek.Shops.Infrastructure/CoffeePeek.Shops.Infrastructure.csproj 
SHOPS_STARTUP   := CoffeePeek.ShopsService/CoffeePeek.ShopsService.csproj 
SHOPS_CONTEXT   := CoffeePeek.Shops.Infrastructure.Configuration.ShopsDbContext

MODERATION_INFRA   := CoffeePeek.Moderation.Infrastructure/CoffeePeek.Moderation.Infrastructure.csproj
MODERATION_STARTUP := CoffeePeek.ModerationService/CoffeePeek.ModerationService.csproj
MODERATION_CONTEXT := CoffeePeek.Moderation.Infrastructure.Configuration.ModerationDbContext

JOBS_INFRA      := CoffeePeek.JobVacancies.Infrastructure/CoffeePeek.JobVacancies.Infrastructure.csproj
JOBS_STARTUP    := CoffeePeek.JobVacancies/CoffeePeek.JobVacancies.csproj
JOBS_CONTEXT    := CoffeePeek.JobVacancies.Infrastructure.Configuration.JobVacanciesDbContext

define add_migration
	dotnet ef migrations add $(4) \
		--project $(1) \
		--startup-project $(2) \
		--context $(3) \
		--configuration Debug \
		--output-dir Migrations
endef

define update_db
	dotnet ef database update \
		--project $(1) \
		--startup-project $(2) \
		--context $(3)
endef

n := UpdEquipments
# Account
mig-acc:
	$(call add_migration,$(ACCOUNT_INFRA),$(ACCOUNT_STARTUP),$(ACCOUNT_CONTEXT),$(n))
up-acc:
	$(call update_db,$(ACCOUNT_INFRA),$(ACCOUNT_STARTUP),$(ACCOUNT_CONTEXT))

# Shops
mig-shops:
	$(call add_migration,$(SHOPS_INFRA),$(SHOPS_STARTUP),$(SHOPS_CONTEXT),$(n))
up-shops:
	$(call update_db,$(SHOPS_INFRA),$(SHOPS_STARTUP),$(SHOPS_CONTEXT))

# Moderation
mig-mod:
	$(call add_migration,$(MODERATION_INFRA),$(MODERATION_STARTUP),$(MODERATION_CONTEXT),$(n))
up-mod:
	$(call update_db,$(MODERATION_INFRA),$(MODERATION_STARTUP),$(MODERATION_CONTEXT))

# Jobs
mig-jobs:
	$(call add_migration,$(JOBS_INFRA),$(JOBS_STARTUP),$(JOBS_CONTEXT),$(n))
up-jobs:
	$(call update_db,$(JOBS_INFRA),$(JOBS_STARTUP),$(JOBS_CONTEXT))