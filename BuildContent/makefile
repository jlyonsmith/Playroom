CONFIG?=Debug
files=Playroom.Compilers.dll Playroom.Compilers.dll.mdb

define copyrule
bin/$$(CONFIG)/$(1): ../Compilers/bin/$$(CONFIG)/$(1)
	cp $$< $$@

endef

all: $(foreach x, $(files), bin/$(CONFIG)/$(x))

$(foreach x, $(files), $(eval $(call copyrule,$(x))))

