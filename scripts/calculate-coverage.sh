# $1 --> The path to the coverage report
# $2 --> What coverage to calculate (e.g. line or branch)
grep -A 1 -i "<th>$2 coverage:<\/th>" $1 | grep -o -E '[[:digit:]]+ of [[:digit:]]+' | awk '{printf "Coverage: %.2f%\n", ($1 / $3)*100}'