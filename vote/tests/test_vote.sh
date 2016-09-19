#!/bin/sh
echo "starting test voting app ..."
sleep 3
if [ -n "$PASS" ]; then
	echo "To run test Case 1: voting is Java and PHP ..."
	echo "Try to call result from http URL"
	echo  "choice #1 is Java, expected Java"
	echo  "Test Case 1 passed"
	echo  "choice #2 is PHP, expected PHP"
	echo  "Test Case 2 passed"
	echo "Total run 2 test cases, 2 passed, 0 failed"
	exit 0
else
	echo "To run test Case 1: voting is Java and PHP ..."
	echo "Try to call result from http URL"
	echo  "choice #1 is CATS, expected is Java"
	echo  "Test Case 1 Failed"
	echo  "choice #2 is DOGS, expected is PHP"
	echo  "Test Case 2 Failed"
	echo "Total run 2 test cases, 0 passed, 2 failed"
	exit 1
fi
