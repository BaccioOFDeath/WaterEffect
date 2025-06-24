import Foundation

class VelocityField {
    var x: [[Float]]
    var y: [[Float]]
    let size: Int

    init(size: Int) {
        self.size = size
        self.x = Array(repeating: Array(repeating: 0.0, count: size), count: size)
        self.y = Array(repeating: Array(repeating: 0.0, count: size), count: size)
    }

    func clear() {
        for i in 0..<size {
            for j in 0..<size {
                x[i][j] = 0
                y[i][j] = 0
            }
        }
    }
}
