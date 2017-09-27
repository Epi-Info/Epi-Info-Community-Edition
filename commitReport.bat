cls
del commitReport.code
git.exe log -50 --pretty=format:%%s > commitReport.code
start commitReport.code
