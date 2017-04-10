package main

import (
	"fmt"
	"os"
	"strconv"
	"strings"
	"sync"
	"time"
)

func main() {
	if len(os.Args) == 1 {
		usage()
	} else {
		runCommand(os.Args[1])
	}
}

// usage prints usage instructions
func usage() {
	var exeName = os.Args[0][strings.LastIndexAny(os.Args[0], "/\\")+1:]

	fmt.Fprintf(os.Stderr, "%s plain\n", exeName)
	fmt.Fprintf(os.Stderr, "%s bidirectional\n", exeName)
	fmt.Fprintf(os.Stderr, "%s buff\n", exeName)
	os.Exit(2)
}

// runCommand runs the specified command
func runCommand(cmd string) {
	switch strings.ToLower(cmd) {
	case "plain":
		averagePerf(plainQueue)
	case "bidirectional":
		averagePerf(bidirectionalQueue)
	case "buff":
		averagePerf(buffQueue)
	default:
		usage()
	}
}

// averagePerf computes the average return value of fn over 10 runs
func averagePerf(fn func() int64) {
	const passes = 10
	var ms int64

	for i := 0; i < passes; i++ {
		var fnMs = fn()
		ms += fnMs
		fmt.Printf("%vms\n", fnMs)
	}

	fmt.Printf("\n %vms avg", ms/passes)
}

// plainQueue tests single-bidirectional channel usage
func plainQueue() int64 {
	const numMessages = 1000000

	var count = 0
	var startTime = time.Now()
	var messageChannel = make(chan string)
	var completionChannel = make(chan int64)

	go func() {
		for {
			<-messageChannel
			count++

			if count >= numMessages {
				completionChannel <- time.Now().Sub(startTime).Nanoseconds() / 1e6
				count = 0
			}
		}
	}()

	for i := 0; i < numMessages; i++ {
		messageChannel <- "Msg " + strconv.Itoa(i)
	}

	return <-completionChannel
}

// buffQueue tests single-directional buffered channel usage
func buffQueue() int64 {
	const numMessages = 1000000

	var wg = sync.WaitGroup{}
	var startTime = time.Now()
	var out = make(chan string, numMessages)

	wg.Add(numMessages)

	go func() {
		for {
			<-out
			wg.Done()
		}
	}()

	for i := 0; i < numMessages; i++ {
		out <- "Msg " + strconv.Itoa(i)
	}

	wg.Wait()

	return time.Now().Sub(startTime).Nanoseconds() / 1e6
}

// bidirectionalQueue tests sending to a channel and awaiting a response
func bidirectionalQueue() int64 {
	const numMessages = 1000000

	var startTime = time.Now()
	var responseChannel = make(chan string)
	var requestChannel = make(chan int)

	go func() {
		for {
			i := <-requestChannel
			responseChannel <- "Hey " + strconv.Itoa(i)
		}
	}()

	for i := 0; i < numMessages; i++ {
		requestChannel <- i
		<-responseChannel
	}

	return time.Now().Sub(startTime).Nanoseconds() / 1e6
}
