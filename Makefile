ACCOUNT_INFRA   := CoffeePeek.Account.Persistence/CoffeePeek.Account.Persistence.csproj 
ACCOUNT_STARTUP := CoffeePeek.AccountService/CoffeePeek.AccountService.csproj
ACCOUNT_CONTEXT := CoffeePeek.Account.Persistence.Configuration.AccountDbContext

SHOPS_INFRA     := CoffeePeek.Shops.Persistance/CoffeePeek.Shops.Persistance.csproj 
SHOPS_STARTUP   := CoffeePeek.ShopsService/CoffeePeek.ShopsService.csproj 
SHOPS_CONTEXT   := CoffeePeek.Shops.Persistance.Configuration.ShopsDbContext

MODERATION_INFRA   := CoffeeShop.Moderation.Persistence/CoffeeShop.Moderation.Persistence.csproj
MODERATION_STARTUP := CoffeePeek.ModerationService/CoffeePeek.ModerationService.csproj
MODERATION_CONTEXT := CoffeeShop.Moderation.Persistence.Configuration.ModerationDbContext

MEDIA_INFRA      := CoffeePeek.MediaService/CoffeePeek.MediaService.csproj
MEDIA_STARTUP    := CoffeePeek.MediaService/CoffeePeek.MediaService.csproj
MEDIA_CONTEXT    := CoffeePeek.MediaService.Data.MediaDbContext

n := AddOutbox3

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
	
# Media
mig-media:
	$(call add_migration,$(MEDIA_INFRA),$(MEDIA_STARTUP),$(MEDIA_CONTEXT),$(n))
up-media:
	$(call update_db,$(MEDIA_INFRA),$(MEDIA_STARTUP),$(MEDIA_CONTEXT))