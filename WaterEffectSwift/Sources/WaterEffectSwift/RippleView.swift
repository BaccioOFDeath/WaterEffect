import UIKit

class RippleView: UIView {
    private let mapSize: Int = 128
    private let initialPressure: Float = 0.7
    private let damping: Float = 0.95

    private var rippleMap: [[Float]]
    private var lastRippleMap: [[Float]]
    private var velocities: VelocityField
    private var displayLink: CADisplayLink?
    private var lastTouchPoints: [UITouch: CGPoint] = [:]
    private var isTouchOccurred = false

    override init(frame: CGRect) {
        self.rippleMap = Array(repeating: Array(repeating: 0, count: mapSize), count: mapSize)
        self.lastRippleMap = self.rippleMap
        self.velocities = VelocityField(size: mapSize)
        super.init(frame: frame)
        commonInit()
    }

    required init?(coder: NSCoder) {
        self.rippleMap = Array(repeating: Array(repeating: 0, count: mapSize), count: mapSize)
        self.lastRippleMap = self.rippleMap
        self.velocities = VelocityField(size: mapSize)
        super.init(coder: coder)
        commonInit()
    }

    private func commonInit() {
        isMultipleTouchEnabled = true
        displayLink = CADisplayLink(target: self, selector: #selector(step))
        displayLink?.add(to: .main, forMode: .common)
    }

    @objc private func step() {
        updateRippleEffect()
        setNeedsDisplay()
    }

    override func touchesBegan(_ touches: Set<UITouch>, with event: UIEvent?) {
        super.touchesBegan(touches, with: event)
        handleTouches(touches, touchBegan: true)
    }

    override func touchesMoved(_ touches: Set<UITouch>, with event: UIEvent?) {
        super.touchesMoved(touches, with: event)
        handleTouches(touches, touchBegan: false)
    }

    override func touchesEnded(_ touches: Set<UITouch>, with event: UIEvent?) {
        super.touchesEnded(touches, with: event)
        removeTouchPoints(touches)
    }

    override func touchesCancelled(_ touches: Set<UITouch>, with event: UIEvent?) {
        super.touchesCancelled(touches, with: event)
        removeTouchPoints(touches)
    }

    private func handleTouches(_ touches: Set<UITouch>, touchBegan: Bool) {
        for touch in touches {
            let location = touch.location(in: self)
            var pressure: Float = 1.0
            if touch.maximumPossibleForce > 0 {
                pressure = Float(touch.force / touch.maximumPossibleForce)
            }
            pressure = min(max(pressure, 0.1), 1.0)
            if touchBegan {
                applyInitialRipple(at: location, pressure: pressure)
            } else if let last = lastTouchPoints[touch] {
                interpolateRipples(from: last, to: location, pressure: pressure)
            }
            lastTouchPoints[touch] = location
        }
    }

    private func removeTouchPoints(_ touches: Set<UITouch>) {
        for touch in touches {
            lastTouchPoints.removeValue(forKey: touch)
        }
    }

    private func applyInitialRipple(at point: CGPoint, pressure: Float) {
        let (x, y) = mapCoordinates(point)
        applyRipple(x: x, y: y, pressure: pressure, damping: damping)
        isTouchOccurred = true
    }

    private func interpolateRipples(from start: CGPoint, to end: CGPoint, pressure: Float) {
        let dynamicDamping = damping
        let steps = Int(max(abs(end.x - start.x), abs(end.y - start.y)))
        for i in 0...steps {
            let t = CGFloat(i) / CGFloat(max(steps, 1))
            let point = CGPoint(x: start.x + t*(end.x - start.x), y: start.y + t*(end.y - start.y))
            let (mx, my) = mapCoordinates(point)
            applyRipple(x: mx, y: my, pressure: pressure, damping: dynamicDamping)
        }
    }

    private func mapCoordinates(_ point: CGPoint) -> (Int, Int) {
        let mapX = Int(point.x * CGFloat(mapSize) / bounds.width)
        let mapY = Int(point.y * CGFloat(mapSize) / bounds.height)
        return (mapX, mapY)
    }

    private func applyRipple(x: Int, y: Int, pressure: Float, damping: Float) {
        let impactRadius = Int(ceil(3 * pressure))
        for dx in -impactRadius...impactRadius {
            for dy in -impactRadius...impactRadius {
                let nx = x + dx
                let ny = y + dy
                if nx >= 1 && nx < mapSize - 1 && ny >= 1 && ny < mapSize - 1 {
                    rippleMap[nx][ny] += initialPressure * pressure / Float(abs(dx)+abs(dy)+1) * damping
                    velocities.x[nx][ny] += Float(dx) * pressure
                    velocities.y[nx][ny] += Float(dy) * pressure
                }
            }
        }
    }

    private func updateRippleEffect() {
        let size = mapSize
        for x in 1..<size-1 {
            for y in 1..<size-1 {
                let gradX = rippleMap[x+1][y] - rippleMap[x-1][y]
                let gradY = rippleMap[x][y+1] - rippleMap[x][y-1]
                velocities.x[x][y] += -gradX * 0.03
                velocities.y[x][y] += -gradY * 0.03
                velocities.x[x][y] *= damping
                velocities.y[x][y] *= damping
                let newHeight = rippleMap[x][y] + (velocities.x[x][y] + velocities.y[x][y]) * 0.5
                lastRippleMap[x][y] = newHeight
            }
        }
        swap(&rippleMap, &lastRippleMap)
    }

    override func draw(_ rect: CGRect) {
        guard let ctx = UIGraphicsGetCurrentContext() else { return }
        ctx.setFillColor(UIColor.black.cgColor)
        ctx.fill(rect)
        let rectWidth = rect.width / CGFloat(mapSize)
        let rectHeight = rect.height / CGFloat(mapSize)
        for x in 0..<mapSize {
            for y in 0..<mapSize {
                let value = rippleMap[x][y]
                let alpha = max(0, min(1, value))
                ctx.setFillColor(UIColor.blue.withAlphaComponent(CGFloat(alpha)).cgColor)
                let r = CGRect(x: CGFloat(x)*rectWidth, y: CGFloat(y)*rectHeight, width: rectWidth, height: rectHeight)
                ctx.fill(r)
            }
        }
    }
}
