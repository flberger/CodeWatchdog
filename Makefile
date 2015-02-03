help:
	@echo 'Targets:'
	@echo '    commit.txt'
	@echo '    commit'
	@echo '    docs'
	@echo '    push'
	@echo '    update_files'

commit.txt:
	hg diff > commit.txt

commit:
	hg commit --logfile commit.txt && rm -f commit.txt

docs:
	doxygen Doxyfile

push:
	hg push 'https://bitbucket.org/flberger/CodeWatchdog'

update_files:
	cp -v CamelCaseCSharpWatchdog.cs Logging.cs Watchdog.cs /cygdrive/g/watchdog_test/Assets/packages/UBS/Dependencies/CodeWatchdog/
	cp -v Test.cs /cygdrive/g/watchdog_test/Assets/scripts/
