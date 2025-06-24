import UIKit

class RippleViewController: UIViewController {
    override func loadView() {
        self.view = RippleView(frame: UIScreen.main.bounds)
    }

    override var prefersStatusBarHidden: Bool { true }
}
