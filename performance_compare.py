import random, time

size = 325
rippleMap = [[random.random() for _ in range(size)] for _ in range(size)]
lastRippleMap = [row[:] for row in rippleMap]


def update_full(ripple, last):
    size = len(ripple)
    for x in range(1, size - 1):
        for y in range(1, size - 1):
            new_height = (
                ripple[x - 1][y] + ripple[x + 1][y] + ripple[x][y - 1] + ripple[x][y + 1]
            ) / 2.0 - last[x][y]
            last[x][y] = new_height * 0.95
    for x in range(size):
        ripple[x], last[x] = last[x], ripple[x]


def update_partial(ripple, last, startx, endx, starty, endy):
    size = len(ripple)
    startx = max(1, startx)
    endx = min(size - 1, endx)
    starty = max(1, starty)
    endy = min(size - 1, endy)
    for x in range(startx, endx):
        for y in range(starty, endy):
            new_height = (
                ripple[x - 1][y] + ripple[x + 1][y] + ripple[x][y - 1] + ripple[x][y + 1]
            ) / 2.0 - last[x][y]
            last[x][y] = new_height * 0.95
    for x in range(size):
        ripple[x], last[x] = last[x], ripple[x]

runs = 20
start = time.perf_counter()
for _ in range(runs):
    update_full(rippleMap, lastRippleMap)
end = time.perf_counter()
full_time = end - start

rippleMap = [[random.random() for _ in range(size)] for _ in range(size)]
lastRippleMap = [row[:] for row in rippleMap]
start = time.perf_counter()
for _ in range(runs):
    update_partial(rippleMap, lastRippleMap, 100, 150, 100, 150)
end = time.perf_counter()
partial_time = end - start

print(f"full:{full_time:.4f} partial:{partial_time:.4f}")
